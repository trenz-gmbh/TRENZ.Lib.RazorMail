using System;
using System.Collections.Generic;
using System.Linq;

namespace TRENZ.Lib.RazorMail.Models;

/// <summary>
/// Represents a collection of mail headers.
/// </summary>
public class MailHeaderCollection : Dictionary<string, object>
{
    /// <summary>
    /// The recipients of the mail message.
    /// </summary>
    public IEnumerable<MailAddress> Recipients
    {
        get => GetAddresses("To");
        set => SetAddresses("To", value);
    }

    /// <summary>
    /// Recipients who will receive a copy of the mail message.
    /// </summary>
    public IEnumerable<MailAddress> CarbonCopy
    {
        get => GetAddresses("CC");
        set => SetAddresses("CC", value);
    }

    /// <summary>
    /// Recipients who will receive a blind carbon copy of the mail message.
    /// </summary>
    public IEnumerable<MailAddress> BlindCarbonCopy
    {
        get => GetAddresses("BCC");
        set => SetAddresses("BCC", value);
    }

    /// <summary>
    /// Addresses that will be used for replies to the mail message.
    /// </summary>
    public IEnumerable<MailAddress> ReplyTo
    {
        get => GetAddresses("Reply-To");
        set => SetAddresses("Reply-To", value);
    }

    /// <summary>
    /// The sender of the mail message.
    /// </summary>
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

    /// <summary>
    /// Adds a recipient to the mail message.
    /// </summary>
    /// <param name="address">The recipient's mail address.</param>
    public void AddRecipient(MailAddress address) => Recipients = Recipients.Append(address);

    /// <summary>
    /// Adds multiple recipients to the mail message.
    /// </summary>
    /// <param name="addresses">The recipients' mail addresses.</param>
    public void AddRecipient(IEnumerable<MailAddress> addresses) => Recipients = Recipients.Concat(addresses);

    /// <summary>
    /// Adds a carbon copy recipient to the mail message.
    /// </summary>
    /// <param name="address">The recipient's mail address.</param>
    public void AddCarbonCopy(MailAddress address) => CarbonCopy = CarbonCopy.Append(address);

    /// <summary>
    /// Adds multiple carbon copy recipients to the mail message.
    /// </summary>
    /// <param name="addresses">The recipients' mail addresses.</param>
    public void AddCarbonCopy(IEnumerable<MailAddress> addresses) => CarbonCopy = CarbonCopy.Concat(addresses);

    /// <summary>
    /// Adds a blind carbon copy recipient to the mail message.
    /// </summary>
    /// <param name="address">The recipient's mail address.</param>
    public void AddBlindCarbonCopy(MailAddress address) => BlindCarbonCopy = BlindCarbonCopy.Append(address);

    /// <summary>
    /// Adds multiple blind carbon copy recipients to the mail message.
    /// </summary>
    /// <param name="addresses">The recipient's mail addresses.</param>
    public void AddBlindCarbonCopy(IEnumerable<MailAddress> addresses) => BlindCarbonCopy = BlindCarbonCopy.Concat(addresses);

    /// <summary>
    /// Adds a reply-to address to the mail message.
    /// </summary>
    /// <param name="address">The reply-to address.</param>
    public void AddReplyTo(MailAddress address) => ReplyTo = ReplyTo.Append(address);

    /// <summary>
    /// Adds multiple reply-to addresses to the mail message.
    /// </summary>
    /// <param name="addresses">The reply-to addresses.</param>
    public void AddReplyTo(IEnumerable<MailAddress> addresses) => ReplyTo = ReplyTo.Concat(addresses);
}
