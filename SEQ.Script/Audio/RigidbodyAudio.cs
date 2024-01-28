// MIT License

using Stride.Engine;
using Stride.Physics;

namespace SEQ.Script
{
    public class RigidbodyAudio : AsyncScript
    {
        public AudioEmitterComponent Emitter;
        public int ResetMs = 100;
        public RigidbodyComponent Rb;
        public string Clip;

        public override async Task Execute()
        {
            while (true) {
                var firstCollision = await Rb.NewCollision();
                var contact = firstCollision.Contacts.First();
                Transform.WorldPosition = contact.PositionOnA + contact.ColliderA.PhysicsWorldTransform.TranslationVector;

                /*var otherCollider = Rb == firstCollision.ColliderA
                    ? firstCollision.ColliderB
                    : firstCollision.ColliderA;*/
                Emitter.Oneshot(Clip);
                await Task.Delay(ResetMs);
            }
        }
    }
}
