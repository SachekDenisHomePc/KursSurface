namespace SurfaceWebApp.Models
{
    public static class DataStorage
    {
        private static SurfaceData _dataStorage;

        public static SurfaceData SurfaceData => _dataStorage ??= new SurfaceData();
    }
}