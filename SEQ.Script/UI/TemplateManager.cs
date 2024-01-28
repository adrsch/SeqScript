using Stride.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SEQ.Script.Core;

namespace SEQ.Script
{
    public abstract class TemplateManager : SyncScript
    {
        public static TemplateManager S;
        public List<Template> Templates = new List<Template>();

        public void StartTemplate(string name)
        {
            foreach (var b in Templates)
            {
                if (b.Name == name)
                {
                    b.Start();
                    return;
                }
            }
        }
        public override void Start()
        {
            S = this;
            base.Start();
        }

        public override void Update()
        {
            foreach (var b in Templates)
            {
                b.Update(Time.deltaTime);
            }
        }
    }
}
