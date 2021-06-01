#!/usr/bin/env bash

# cbackup: Simple full app + data + metadata backup/restore script for Android
#
# Required Termux packages: tsu tar sed zstd openssl-tool
# Optional packages: pv
#
# App data backups are tarballs compressed with Zstandard and encrypted with
# AES-256-CTR.
#
# Licensed under the MIT License (MIT)
#
# Copyright (c) 2020 Danny Lin <danny@kdrag0n.dev>
#
# Permission is hereby granted, free of charge, to any person obtaining a copy
# of this software and associated documentation files (the "Software"), to deal
# in the Software without restriction, including without limitation the rights
# to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
# copies of the Software, and to permit persons to whom the Software is
# furnished to do so, subject to the following conditions:
#
# The above copyright notice and this permission notice shall be included in all
# copies or substantial portions of the Software.
#
# THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
# IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
# FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
# AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
# LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
# OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
# SOFTWARE.

set -euo pipefail
shopt -s nullglob dotglob extglob

# Constants
BACKUP_VERSION="0"
PASSWORD_CANARY="cbackup-valid"

# Settings
tmp_dir="/data/local/tmp/._cbackup_tmp"
backup_dir="${2:-/sdcard/cbackup}"
#password="$(cat /data/user/0/com.kaname.artemiscompanion/files/configs/pass.txt)"
encryption_args=(-pbkdf2 -iter 200001 -aes-256-ctr)
debug=false
# WARNING: Hardcoded password FOR TESTING ONLY!
# password="cbackup-test!"
# Known broken/problemtic apps to ignore entirely
app_blacklist=(
    # Restoring Magisk Manager may cause problems with root access
    com.topjohnwu.magisk
	com.kaname.artemiscompanion
	com.termux
)

pass_file="/data/user/0/com.kaname.artemiscompanion/files/configs/pass.txt"
file_content=$(cat "${pass_file}")

if [ -f "$pass_file" ]; then
		password="$file_content"
	else
		exit 1
fi

# Select default action based on filename, because we self-replicate to restore.sh in backups
action="${1:-$([[ "$0" == *"restore"* ]] && echo restore || echo backup)}"

# Prints an error in bold red and exits the script
function die() {
    exit 1
}

function ask_password() {
	read -d $'\x04' password < "/data/user/0/com.kaname.artemiscompanion/files/configs/pass.txt"
}

function encrypt_to_file() {
    PASSWORD="$password" openssl enc -out "$1" "${encryption_args[@]}" -pass env:PASSWORD
}

function decrypt_file() {
    PASSWORD="$password" openssl enc -d -in "$1" "${encryption_args[@]}" -pass env:PASSWORD
}

function expect_output() {
    grep -v "$@" || true
}

function parse_diskstats_array() {
    local diskstats="$1"
    local label="$2"

    grep "$label: " <<< "$diskstats" | \
        sed "s/$label: //" | \
        tr -d '"[]' | \
        tr ',' '\n'
}

function get_app_data_sizes() {
    local diskstats pkg_names data_sizes end_idx
    declare -n size_map="$1"

    diskstats="$(dumpsys diskstats)"
    mapfile -t pkg_names < <(parse_diskstats_array "$diskstats" "Package Names")
    mapfile -t data_sizes < <(parse_diskstats_array "$diskstats" "App Data Sizes")
    end_idx="$((${#data_sizes[@]} - 1))"

    for i in $(seq 0 $end_idx)
    do
        # This is a name reference that should be used by the caller.
        # shellcheck disable=SC2034
        size_map["${pkg_names[$i]}"]="${data_sizes[$i]}"
    done
}

# Setup
ssaid_restored=false
termux_restored=false
android_version="$(getprop ro.build.version.release | cut -d'.' -f1)"
rm -fr "$tmp_dir"
mkdir -p "$tmp_dir"

function do_backup() {
    rm -rf "$backup_dir"
    mkdir -p "$backup_dir"
	
    # Get list of user app package names
    pm list packages --user 0 > "$tmp_dir/pm_all_pkgs.list"
    pm list packages -s --user 0 > "$tmp_dir/pm_sys_pkgs.list"
    local apps
    apps="$(grep -vf "$tmp_dir/pm_sys_pkgs.list" "$tmp_dir/pm_all_pkgs.list" | sed 's/package://g')"

    # Remove ignored apps
    tr ' ' '\n' <<< "${app_blacklist[*]}" > "$tmp_dir/pm_ignored.list"
    apps="$(grep -vf "$tmp_dir/pm_ignored.list" <<< "$apps")"

    # Get map of app data sizes
    declare -A app_data_sizes
    get_app_data_sizes app_data_sizes

    # Back up apps
    local app
    for app in $apps
    do
        local app_out app_info
        app_out="$backup_dir/$app"
        mkdir "$app_out"
        app_info="$(dumpsys package "$app")"

        # cbackup metadata
        echo "$BACKUP_VERSION" > "$app_out/backup_version.txt"
        echo -n "$PASSWORD_CANARY" | encrypt_to_file "$app_out/password_canary.enc"

        # APKs
        mkdir "$app_out/apk"
        local apk_dir
        apk_dir="$(grep "codePath=" <<< "$app_info" | sed 's/^\s*codePath=//')"
        cp "$apk_dir/"*.apk "$app_out/apk"

        # Data
        pushd / > /dev/null

        # Collect list of files
        local files=(
            # CE data for user 0
            "data/data/$app/"!(@(cache|code_cache|no_backup)) \
            # DE data for user 0
            "data/user_de/0/$app/"!(@(cache|code_cache|no_backup))
        )

        # Skip backup if file list is empty
        if [[ ! ${#files[@]} -eq 0 ]]; then
			if [[ "$android_version" -ge 9 ]]; then
                pm suspend --user 0 "$app" | expect_output 'new suspended state: true'
                suspended=true
            fi
		
            # Finally, perform backup if we have files to back up
            tar -cf - "${files[@]}" | \
                zstd -T0 - | \
                encrypt_to_file "$app_out/data.tar.zst.enc"
				
			if $suspended; then
                pm unsuspend --user 0 "$app" | expect_output 'new suspended state: false'
            fi
        fi

        popd > /dev/null

        # Permissions
        grep "granted=true, flags=" <<< "$app_info" | \
            sed 's/^\s*\(.*\): granted.*$/\1/g' | \
            sort | \
            uniq > "$app_out/permissions.list" \
            || true

        # SSAID
        if grep -q 'package="'"$app"'"' /data/system/users/0/settings_ssaid.xml; then
            grep 'package="'"$app"'"' /data/system/users/0/settings_ssaid.xml > "$app_out/ssaid.xml"
        fi

        # Installer name
        if grep -q "installerPackageName=" <<< "$app_info"; then
            grep "installerPackageName=" <<< "$app_info" | \
                sed 's/^\s*installerPackageName=//' > "$app_out/installer_name.txt"
        fi
    done
}

function do_restore() {
    # First pass to show the user a list of apps to restore
    local apps=()
    local app_dir
    for app_dir in "$backup_dir/"*
    do
        if [[ ! -d "$app_dir" ]]; then
            continue
        fi

        app="$(basename "$app_dir")"
        apps+=("$app")
    done

    tr ' ' '\n' <<< "${apps[@]}"

    local installed_apps
    installed_apps="$(pm list packages --user 0 | sed 's/package://g')"

    local app
    for app in "${apps[@]}"
    do
        local app_dir="$backup_dir/$app"

        # Check version
        if [[ ! -f "$app_dir/backup_version.txt" ]]; then
            die "Backup version is missing"
        else
            local bver
            bver="$(cat "$app_dir/backup_version.txt")"
            if [[ "$bver" != "$BACKUP_VERSION" ]]; then
                die "Incompatible backup version $bver, expected $BACKUP_VERSION"
            fi
        fi

        # Check password canary
        if [[ "$(decrypt_file "$app_dir/password_canary.enc")" != "$PASSWORD_CANARY" ]]; then
            die "Incorrect password or corrupted backup!"
        fi

        # APKs
        local suspended=false
            # Proceed with APK installation

            # Uninstall old app if already installed
            # We don't just clear data because there are countless other Android
            # metadata values that are hard to clean: SSAIDs, permissions, special
            # permissions, etc.
            if grep -q "$app" <<< "$installed_apps"; then
                pm uninstall --user 0 "$app" | expect_output Success
            fi

            # Prepare to invoke pm install
            local pm_install_args=(
                # Allow test packages (i.e. ones installed by Android Studio's "Run" button)
                -t
                # Only install for user 0
                --user 0
                # Set expected package name
                --pkg "$app"
            )

            # Installed due to device restore (on Android 10+)
            if [[ "$android_version" -ge 10 ]]; then
                pm_install_args+=(--install-reason 2)
            fi

            # Installer name
            if [[ -f "$app_dir/installer_name.txt" ]]; then
                pm_install_args+=(-i "$(cat "$app_dir/installer_name.txt")")
            fi

            # Install split APKs
            local pm_session
            pm_session="$(pm install-create "${pm_install_args[@]}" | sed 's/^.*\[\([[:digit:]]*\)\].*$/\1/')"

            local apk
            for apk in "$app_dir/apk/"*
            do
                # We need to specify size because we're streaming it to pm through stdin
                # to avoid creating a temporary file
                local apk_size split_name
                apk_size="$(wc -c "$apk" | cut -d' ' -f1)"
                split_name="$(basename "$apk")"

                cat "$apk" | pm install-write -S "$apk_size" "$pm_session" "$split_name" | expect_output Success
            done

            pm install-commit "$pm_session" | expect_output Success
            if [[ "$android_version" -ge 9 ]]; then
                pm suspend --user 0 "$app" | expect_output 'new suspended state: true'
                suspended=true
            fi

        # Get info of newly installed app
        local app_info
        app_info="$(dumpsys package "$app")"

        # Data
        local data_dir="/data/data/$app"
        local de_data_dir="/data/user_de/0/$app"

        # We can't delete and extract directly to the Termux root because we
        # need to use Termux-provided tools for extracting app data
        local out_root_dir
            out_root_dir="/"

        # Create new data directory for in-place Termux restore
        # No extra slash here because both are supposed to be absolute paths
        local new_data_dir="$out_root_dir$data_dir"
        mkdir -p "$new_data_dir"
        chmod 700 "$new_data_dir"

        # Get UID and GIDs
        local uid
        uid="$(grep "userId=" <<< "$app_info" | head -1 | sed 's/^\s*userId=//')"
        local gid_cache="$((uid + 10000))"

        # Get SELinux context from the system-created data directory
        # Parsing the output of ls is not ideal, but Termux doesn't come with any
        # tools for this.
        # TODO: Fix the sporadic failure codes instead of silencing them with a declaration
        # shellcheck disable=SC2012
        local secontext="$(/system/bin/ls -a1Z "$data_dir" | head -1 | cut -d' ' -f1)"

        # Finally, extract the app data
        local data_archive="$app_dir/data.tar.zst.enc"
        if [[ -f "$data_archive" ]]; then
            decrypt_file "$app_dir/data.tar.zst.enc" | \
                zstd -d -T0 - | \
                tar -C "$out_root_dir" -xf -
        fi

        # Fix ownership
        chown -R "$uid:$uid" "$new_data_dir" "$de_data_dir"
        local cache_dirs=("$new_data_dir/"*cache* "$de_data_dir/"*cache*)
        if [[ ${#cache_dirs[@]} -ne 0 ]]; then
            chown -R "$uid:$gid_cache" "$new_data_dir/"*cache* "$de_data_dir/"*cache*
        fi

        # Fix SELinux context
        # We need to use Android chcon to avoid "Operation not supported on transport endpoint" errors
        /system/bin/chcon -hR "$secontext" "$new_data_dir" "$de_data_dir"

        # Permissions
        local perm
        for perm in $(cat "$app_dir/permissions.list")
        do
            pm grant --user 0 "$app" "$perm" || warn "Failed to grant permission $perm!"
        done

        # SSAID
        if [[ -f "$app_dir/ssaid.xml" ]]; then
            cat "$app_dir/ssaid.xml" >> /data/system/users/0/settings_ssaid.xml
            ssaid_restored=true
        fi

        # Unsuspend app now that restoration is finished
        if $suspended; then
            pm unsuspend --user 0 "$app" | expect_output 'new suspended state: false'
        fi
    done
}

# Run action
su
export HOME=/data/data/com.termux/files/home
export TMPDIR=/data/data/com.termux/files/home/.tmp
export LD_PRELOAD=/data/data/com.termux/files/usr/lib/libtermux-exec.so
export PATH=/data/data/com.termux/files/usr/bin:/data/data/com.termux/files/usr/bin/applets:/system/bin:/system/xbin:/sbin:/sbin/bin
if [[ "$action" == "backup" ]]; then
    do_backup
elif [[ "$action" == "restore" ]]; then
    do_restore
else
    die "Unknown action '$action'"
fi

# Cleanup
rm -fr "$tmp_dir"

exit 0
