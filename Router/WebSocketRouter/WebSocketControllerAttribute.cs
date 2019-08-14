namespace ICoaster.Router.WebSocketRouter
{
    [System.AttributeUsage(System.AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    sealed class WebSocketControllerAttribute : System.Attribute
    {
        public string Path { get; set; }
    }


    [System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    sealed class SubPathAttribute : System.Attribute
    {
        public string Path { get; set; }
    }
}