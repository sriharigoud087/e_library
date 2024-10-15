namespace E_LibraryManager.Utilties
{
    public static class Util {

        public static string GetRandomNumber() {
            Random rnd = new Random();
            int myRandomNo = rnd.Next(10000000, 99999999);
            return myRandomNo.ToString();
        }
    
    }
}
