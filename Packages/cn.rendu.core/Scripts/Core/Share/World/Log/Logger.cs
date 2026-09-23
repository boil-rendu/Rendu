namespace RD.Core
{
    public class Logger: Singleton<Logger>, ISingletonAwake
    {
        private ILog log;

        public ILog Log
        {
            set
            {
                this.log = value;
            }
            get
            {
                return this.log;
            }
        }
        
        public void Awake()
        {
        }
    }
}