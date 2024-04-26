using System;
using System.Collections.Generic;
using System.Linq;

namespace TRENZ.Lib.RazorMail.Models;

public class MailHeaderCollection : Dictionary<string, object>
{
    public IEnumerable<MailAddress> Recipients
    {
        get => GetAddresses("To");
        set => SetAddresses("To", value);
    }

    public IEnumerable<MailAddress> CarbonCopy
    {
        get => GetAddresses("CC");
        set => SetAddresses("CC", value);
    }

    public IEnumerable<MailAddress> BlindCarbonCopy
    {
        get => GetAddresses("BCC");
        set => SetAddresses("BCC", value);
    }

    public MailAddress? From
    {
        get
        {
            if (ContainsKey("From"))
                return this["From"] as MailAddress;

            return null;
        }
        set
        {
            if (value is null)
                Remove("From");
            else
                this["From"] = value;
        }
    }

    private IEnumerable<MailAddress> GetAddresses(string key)
    {
        if (!ContainsKey(key))
            return Array.Empty<MailAddress>();

        return (this[key] as IEnumerable<MailAddress>)!;
    }

    private void SetAddresses(string key, IEnumerable<MailAddress> value) => this[key] = value;

    public void AddRecipient(MailAddress address) => Recipients = Recipients.Append(address);

    public void AddCarbonCopy(MailAddress address) => CarbonCopy = CarbonCopy.Append(address);

    public void AddBlindCarbonCopy(MailAddress address) => BlindCarbonCopy = BlindCarbonCopy.Append(address);
}
