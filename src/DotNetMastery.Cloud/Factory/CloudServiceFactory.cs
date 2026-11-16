using DotNetMastery.Cloud.Abstractions;
using DotNetMastery.Cloud.Aws;
using DotNetMastery.Cloud.Azure;
using DotNetMastery.Cloud.Gcp;

namespace DotNetMastery.Cloud.Factory;

public interface ICloudServiceFactory
{
    IBlobStorageService GetBlobStorage(CloudProvider provider);
    ISecretManagerService GetSecretManager(CloudProvider provider);
    IMessageQueueService GetMessageQueue(CloudProvider provider);
    ICloudIdentityService GetIdentityService(CloudProvider provider);
}

/// <summary>
/// Central factory allowing multi-cloud or hybrid-cloud services to be resolved dynamically
/// based on runtime configuration or per-tenant policies.
/// </summary>
public class CloudServiceFactory : ICloudServiceFactory
{
    private readonly Dictionary<CloudProvider, IBlobStorageService> _blobServices = new();
    private readonly Dictionary<CloudProvider, ISecretManagerService> _secretServices = new();
    private readonly Dictionary<CloudProvider, IMessageQueueService> _queueServices = new();
    private readonly Dictionary<CloudProvider, ICloudIdentityService> _identityServices = new();

    public CloudServiceFactory()
    {
        // Register Azure implementations
        _blobServices[CloudProvider.Azure] = new AzureBlobStorageService();
        _secretServices[CloudProvider.Azure] = new AzureKeyVaultSecretService();
        _queueServices[CloudProvider.Azure] = new AzureServiceBusQueueService();
        _identityServices[CloudProvider.Azure] = new AzureManagedIdentityService();

        // Register AWS implementations
        _blobServices[CloudProvider.Aws] = new AwsS3StorageService();
        _secretServices[CloudProvider.Aws] = new AwsSecretsManagerService();
        _queueServices[CloudProvider.Aws] = new AwsSqsQueueService();
        _identityServices[CloudProvider.Aws] = new AwsIamIdentityService();

        // Register GCP implementations
        _blobServices[CloudProvider.Gcp] = new GcpCloudStorageService();
        _secretServices[CloudProvider.Gcp] = new GcpSecretManagerService();
        _queueServices[CloudProvider.Gcp] = new GcpPubSubQueueService();
        _identityServices[CloudProvider.Gcp] = new GcpWorkloadIdentityService();
    }

    public IBlobStorageService GetBlobStorage(CloudProvider provider) => _blobServices[provider];

    public ISecretManagerService GetSecretManager(CloudProvider provider) => _secretServices[provider];

    public IMessageQueueService GetMessageQueue(CloudProvider provider) => _queueServices[provider];

    public ICloudIdentityService GetIdentityService(CloudProvider provider) => _identityServices[provider];
}
