namespace GymApp_v1.Data
{
    /// Uygulamada kullanılacak üyelik türlerini tanımlar.
    public enum MembershipType
    {   
        None    = 0,  // “Seçilmedi” ya da başlangıç değeri
        Pool = 1,    // Havuz üyeliği
        Fitness = 2, // Fitness üyeliği
        Gold = 3     // Gold üyelik
    }
}
