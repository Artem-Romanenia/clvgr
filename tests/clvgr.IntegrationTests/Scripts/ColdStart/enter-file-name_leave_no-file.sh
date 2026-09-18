#!/bin/sh
. "$(dirname "$0")/../_base.sh"

tui-test run /app/clvgr || { tui-test close; exit 1; }

step tui-test expect text "Type [Alt+F,S] to create/open secrets file."
step tui-test click text "File"
step tui-test click text "Add/Open"
step tui-test expect text "Select Secrets file"
step tui-test submit "/my/dir/secrets.clvgr"
step tui-test expect text "Master Password"
step tui-test key press Esc
step tui-test expect text "Master Password" --not
step tui-test expect text "File" 
step tui-test key press Esc

tui-test wait exit --timeout 5000
tui-test close
