namespace ShipmentService.Application.Constants;

public static class ShipmentMessages
{
    // Shipment
    public const string ShipmentNotFound = "Shipment not found";
    public const string ShipmentCreated = "Shipment created successfully";
    public const string ShipmentUpdated = "Shipment updated successfully";
    public const string ShipmentDeleted = "Shipment deleted successfully";
    public const string ShipmentStatusUpdated = "Shipment status updated successfully";
    public const string ShipmentCreateError = "Error creating shipment";
    public const string ShipmentUpdateError = "Error updating shipment";
    public const string ShipmentDeleteError = "Error deleting shipment";

    // ShippingProvider
    public const string ProviderNotFound = "Shipping provider not found";
    public const string ProviderCreated = "Provider created successfully";
    public const string ProviderUpdated = "Shipping provider updated successfully";
    public const string ProviderDeleted = "Shipping provider deleted successfully";
    public const string ProviderCreateError = "Error creating shipping provider";
    public const string ProviderUpdateError = "Error updating shipping provider";
    public const string ProviderDeleteError = "Error deleting shipping provider";
    public const string ProviderNameDuplicate = "A provider with this name already exists";
    public const string ProviderHasActiveServices = "Cannot deactivate provider: it still has active services. Deactivate all associated services first.";
    public const string ProviderHasActiveShipments = "Cannot deactivate provider: there are shipments in non-terminal status using this provider.";

    // ProviderService
    public const string ServiceNotFound = "Provider service not found";
    public const string ServiceCreated = "Provider service created successfully";
    public const string ServiceUpdated = "Provider service updated successfully";
    public const string ServiceDeleted = "Provider service deleted successfully";
    public const string ServiceCreateError = "Error creating provider service";
    public const string ServiceUpdateError = "Error updating provider service";
    public const string ServiceDeleteError = "Error deleting provider service";
    public const string ServiceHasActiveShipments = "Cannot deactivate service: there are shipments in non-terminal status using this service.";
}
