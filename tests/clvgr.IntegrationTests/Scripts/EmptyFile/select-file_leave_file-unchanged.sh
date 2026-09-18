#!/bin/sh
. "$(dirname "$0")/../_base.sh"

tui-test run /app/clvgr || { tui-test close; exit 1; }

step tui-test click text "File"
step tui-test click text "Add/Open"
step tui-test expect text "Select Secrets file"
# At this point dialog window is opened, and cursor is in file path field, and text is preselected
step tui-test submit "/files/empty.clvgr"
step tui-test expect text "Master Password"
step tui-test submit "password"
step tui-test expect text "Master Password" --not
step tui-test expect text "Error" --not
step tui-test expect text "File" # We're on the main screen again
step tui-test key press Esc

step tui-test wait exit --timeout 5000
step tui-test close
