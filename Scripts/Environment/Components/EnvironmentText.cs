using Revistone.Console.Image;

namespace Revistone
{
    namespace Environment
    {
        public class EnvironmentText : EnvironmentComponent
        {
            public string text { get; private set; }

            public int textOrder;

            public EnvironmentText(string text, int textOrder = 0)
            {
                this.text = text;
                this.textOrder = textOrder;
            }

            public void UpdateText(string text)
            {
                this.text = text;
            }
        }
    }
}