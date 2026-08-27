using UnityEngine.Purchasing;

namespace GoldAndGoblins.Economy
{
    // Ship a real implementation before launch: send the receipt to your own backend
    // (or Unity Cloud Save / Play Integrity / App Store Server API) and confirm it
    // server-side before granting premium currency. The default implementation below
    // trusts the client, which is fine for early development but not for production.
    public interface IReceiptValidator
    {
        bool Validate(Product product);
    }

    public class TrustClientReceiptValidator : IReceiptValidator
    {
        public bool Validate(Product product) => true;
    }
}
