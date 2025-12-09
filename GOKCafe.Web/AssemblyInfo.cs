using Gotik.Commerce.Composing;
using Umbraco.Cms.Core.Composing;

// Disable the Gotik.Commerce package composer to prevent duplicate service registrations
[assembly: DisableComposer(typeof(GotikCommerceComposer))]
