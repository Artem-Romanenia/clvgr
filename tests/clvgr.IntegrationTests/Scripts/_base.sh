STEP=0

step() {
    STEP=$((STEP + 1))
    "$@" || {
        echo "Action failed at step $STEP: $*" >&2
        tui-test close
        exit "$STEP"
    }
}
