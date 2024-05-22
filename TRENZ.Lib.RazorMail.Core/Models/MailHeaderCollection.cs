using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace TRENZ.Lib.RazorMail.Models;

/// <summary>
/// Represents a collection of mail headers.
/// </summary>
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global",
    Justification = "Keys may be checked from outside the class.")]
public class MailHeaderCollection() : Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
{
    /// <summary>
    /// The key for the "To" address.
    /// </summary>
    public const string ToKey = "To";

    /// <summary>
    /// The key for the "CC" address.
    /// </summary>
    public const string CcKey = "CC";

    /// <summary>
    /// The key for the "BCC" address.
    /// </summary>
    public const string BccKey = "BCC";

    /// <summary>
    /// The key for the "Reply-To" address.
    /// </summary>
    public const string ReplyToKey = "Reply-To";

    /// <summary>
    /// The key for the "From" address.
    /// </summary>
    public const string FromKey = "From";

    /// <summary>
    /// The keys for the mail addresses in the collection.
    /// </summary>
    public static readonly string[] AddressKeys = [ToKey, CcKey, BccKey, ReplyToKey, FromKey];

    /// <summary>
    /// The recipients of the mail message.
    /// </summary>
    public IEnumerable<MailAddress> Recipients
    {
        get => GetAddresses(ToKey);
        set => SetAddresses(ToKey, value);
    }

    /// <summary>
    /// Recipients who will receive a copy of the mail message.
    /// </summary>
    public IEnumerable<MailAddress> CarbonCopy
    {
        get => GetAddresses(CcKey);
        set => SetAddresses(CcKey, value);
    }

    /// <summary>
    /// Recipients who will receive a blind carbon copy of the mail message.
    /// </summary>
    public IEnumerable<MailAddress> BlindCarbonCopy
    {
        get => GetAddresses(BccKey);
        set => SetAddresses(BccKey, value);
    }

    /// <summary>
    /// Addresses that will be used for replies to the mail message.
    /// </summary>
    public IEnumerable<MailAddress> ReplyTo
    {
        get => GetAddresses(ReplyToKey);
        set => SetAddresses(ReplyToKey, value);
    }

    /// <summary>
    /// The sender of the mail message.
    /// </summary>
    public MailAddress? From
    {
        get
        {
            if (TryGetValue(FromKey, out var value) && value is MailAddress address)
                return address;

            return null;
        }
        set
        {
            if (value is null)
                Remove(FromKey);
            else
                this[FromKey] = value;
        }
    }

    private IEnumerable<MailAddress> GetAddresses(string key)
    {
        if (!TryGetValue(key, out var value) || value is not IEnumerable<MailAddress> addresses)
            return Array.Empty<MailAddress>();

        return addresses;
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
    public void AddBlindCarbonCopy(IEnumerable<MailAddress> addresses) =>
        BlindCarbonCopy = BlindCarbonCopy.Concat(addresses);

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

    /// <summary>
    /// Gets the headers that are not mail addresses and therefore have no getter/setter.
    /// </summary>
    public IReadOnlyDictionary<string, object> NonAddressHeaders => this
        .ExceptBy(AddressKeys, x => x.Key)
        .ToDictionary(x => x.Key, x => x.Value);

    /// <summary>
    /// Appends <paramref name="other"/> to this collection without overwriting existing values.
    /// </summary>
    /// <param name="other">The collection to append.</param>
    /// <returns>The current instance.</returns>
    public MailHeaderCollection AppendRange(MailHeaderCollection other)
    {
        From ??= other.From;

        AddRecipient(other.Recipients);
        AddCarbonCopy(other.CarbonCopy);
        AddBlindCarbonCopy(other.BlindCarbonCopy);
        AddReplyTo(other.ReplyTo);

        foreach (var (key, value) in other.NonAddressHeaders)
            if (!ContainsKey(key))
                this[key] = value;

        return this;
    }

    /// <summary>
    /// Overwrites the values of this collection with the values from <paramref name="other"/>.
    /// </summary>
    /// <param name="other">The collection to overwrite with.</param>
    /// <returns>The current instance.</returns>
    public MailHeaderCollection OverwriteWith(MailHeaderCollection other)
    {
        foreach (var (key, value) in other)
            this[key] = value;

        return this;
    }
}
