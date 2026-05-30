namespace Wasel.Api.Modules.Wallets.Entities;

public class PlatformWallet : Wallet
{
    // Identifiant unique facultatif si plusieurs wallets plateforme, sinon Id unique suffit.
    // Utilisé pour les commissions, remboursements, ajustements.
    public string Name { get; set; } = "Main Platform Wallet";
}
