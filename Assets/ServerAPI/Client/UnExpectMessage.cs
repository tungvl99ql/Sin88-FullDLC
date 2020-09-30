
namespace Core.Server.Api
{


    public class UnExpectMessage
    {

        public string errorType;
        public string errorContent;
        public UnExpectMessage(string _type, string _content)
        {
            this.errorType = _type;
            this.errorContent = _content;
        }

        public override string ToString()
        {
            return "error type: " + errorType + " error content " + errorContent;
        }
    }

}