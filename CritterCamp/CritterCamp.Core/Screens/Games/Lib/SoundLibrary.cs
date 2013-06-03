using Microsoft.Xna.Framework.Audio;
using System.Collections.Generic;

namespace CritterCamp.Core.Screens.Games.Lib {
    public class SoundLibrary {
        protected List<SoundEffectInstance> loopedSounds = new List<SoundEffectInstance>();

        public SoundLibrary() {

        }

        public void StopAllSounds() {
            foreach(SoundEffectInstance se in loopedSounds) {
                se.Stop();
            }
            loopedSounds.Clear();
        }

        public void LoopSound(SoundEffectInstance se) {
            loopedSounds.Add(se);
            se.IsLooped = true;
            se.Play();
        }

        public void RemoveSound(SoundEffectInstance se) {
            se.Stop();
            loopedSounds.Remove(se);
        }
    }
}
