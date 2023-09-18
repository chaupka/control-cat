namespace TheKiwiCoder
{
    public class Sequencer : CompositeNode {
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
                    case State.Failure:
                        return State.Failure;
                    case State.Success:
                        continue;
                }
            }

            return State.Success;
        }
    }
}