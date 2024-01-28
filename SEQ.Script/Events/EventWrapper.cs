using System.Collections;
using System.Collections.Generic;
using System;
using Stride.Engine;
using Stride.Core;
using Stride.Core.Serialization.Contents;
using SEQ.Script.Core;

namespace SEQ.Script
{
    // TODO: remove this
    // legacy code from unity
    [DataContract]
    [ContentSerializer(typeof(DataContentSerializer<EventWrapper>))]

    public class EventWrapper
    {
        public Action Event;
        //     public UnityEvent Event;
        [DataMember]
        public string Seq;
        //   public Animation Anim;
        [DataMember]
        public string Clip;
        public void Invoke(TransformComponent t)
        {
            Event?.Invoke();
   //         Event.Invoke();
            Shell.Exec(Seq);
       //     if (Anim != null && !string.IsNullOrWhiteSpace(Clip))
   //         {
    //            Anim.Play(Clip);
    //        }
        }
    }
}