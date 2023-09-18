namespace TheKiwiCoder
{
    public class Selector : CompositeNode {
        protected int currentBranch;

        protected override void OnStart() {
            currentBranch = 0;
        }

        protected override void OnStop() {
        }

        protected override State OnUpdate() {
            for (int i = currentBranch; i < children.Count; ++i) {
                currentBranch = i;
                var child = children[currentBranch];

                switch (child.Update()) {
                    case State.Running:
                        return State.Running;
                    case State.Success:
                        return State.Success;
                    case State.Failure:
                        continue;
                }
            }

            return State.Failure;
        }
    }
}