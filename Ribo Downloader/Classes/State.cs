namespace Ribo_Downloader.Core
{
    public static class State
    {
        public const int Create = 0;
        public const int Idle = 1;
        public const int Start = 2;
        public const int Download = 3;
        public const int Append = 4;
        public const int Complete = 5;
        public const int Error = 6;
        public const int Abort = 7;
}
}
