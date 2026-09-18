#!/bin/sh
tui-test run /app/clvgr
tui-test expect text "Type [Alt+F,S] to create/open secrets file." || { tui-test close; exit 1; }
tui-test close
