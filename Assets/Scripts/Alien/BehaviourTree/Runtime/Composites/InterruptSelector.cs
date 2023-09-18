namespace TheKiwiCoder
{
    public class InterruptSelector : Selector {
        protected override State OnUpdate() {
            int previous = currentBranch;
            base.OnStart();
            var status = base.OnUpdate();
            if (previous != currentBranch) {
                if (children[previous].state == State.Running) {
                    children[previous].Abort();
                }
            }

            return status;
        }
    }
}