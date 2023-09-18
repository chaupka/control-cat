using UnityEngine;

namespace TheKiwiCoder
{
    public class RandomSelector : CompositeNode {
        protected int currentBranch;

        protected override void OnStart() {
            currentBranch = Random.Range(0, children.Count);
        }

        protected override void OnStop() {
        }

        protected override State OnUpdate() {
            var child = children[currentBranch];
            return child.Update();
        }
    }
}