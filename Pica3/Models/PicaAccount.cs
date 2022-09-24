namespace Pica3.Models;

public class PicaAccount
{

    public string Account { get; set; }

    public string? Password { get; set; }

    public bool Selected { get; set; }

    public bool AutoLogin { get; set; }


    public bool IsEnecrypted { get; private set; }

    public bool IsDecrypted { get; private set; }


    public void Encrypt()
    {
        if (IsEnecrypted)
        {
            return;
        }
        try
        {
            if (!string.IsNullOrWhiteSpace(Account))
            {
                Account = Convert.ToHexString(AesHelper.Encrypt(Account));
            }
            if (!string.IsNullOrWhiteSpace(Password))
            {
                Password = Convert.ToHexString(AesHelper.Encrypt(Password));
            }
            IsEnecrypted = true;
        }
        catch { }
    }


    public void Decrypt()
    {
        if (IsDecrypted)
        {
            return;
        }
        try
        {
            if (!string.IsNullOrWhiteSpace(Account))
            {
                Account = AesHelper.Decrypt(Convert.FromHexString(Account));
            }
            if (!string.IsNullOrWhiteSpace(Password))
            {
                Password = AesHelper.Decrypt(Convert.FromHexString(Password));
            }
            IsDecrypted = true;
        }
        catch { }
    }

}
