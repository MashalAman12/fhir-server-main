﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;
using Hl7.Fhir.Model;
using Task = System.Threading.Tasks.Task;

namespace Microsoft.Health.Fhir.Client
{
    public static class FhirClientExtensions
    {
        public static async Task DeleteAllResources(this FhirClient client, ResourceType resourceType)
        {
            await DeleteAllResources(client, resourceType, null);
        }

        public static async Task DeleteAllResources(this FhirClient client, ResourceType resourceType, string searchUrl)
        {
            Bundle bundle = null;
            while (bundle == null || bundle.NextLink != null)
            {
                bundle = bundle == null ? await client.SearchAsync(resourceType, searchUrl, count: 100) : await client.SearchAsync(bundle.NextLink.ToString());

                foreach (Bundle.EntryComponent entry in bundle.Entry)
                {
                    await client.DeleteAsync(entry.FullUrl);
                }
            }
        }

        public static async Task<TResource[]> CreateResourcesAsync<TResource>(this FhirClient client, int count)
            where TResource : Resource, new()
        {
            TResource[] resources = new TResource[count];

            for (int i = 0; i < resources.Length; i++)
            {
                TResource resource = new TResource();

                resources[i] = await client.CreateAsync(resource);
            }

            return resources;
        }

        public static async Task<TResource> CreateResourcesAsync<TResource>(this FhirClient client, Func<TResource> resourceFactory)
            where TResource : Resource
        {
            TResource resource = resourceFactory();

            return await client.CreateAsync(resource);
        }

        public static async Task<TResource[]> CreateResourcesAsync<TResource>(this FhirClient client, params Action<TResource>[] resourceCustomizer)
            where TResource : Resource, new()
        {
            TResource[] resources = new TResource[resourceCustomizer.Length];

            for (int i = 0; i < resources.Length; i++)
            {
                TResource resource = new TResource();

                resourceCustomizer[i](resource);

                resources[i] = await client.CreateAsync(resource);
            }

            return resources;
        }

        /// <summary>
        /// Performs a create by calling <see cref="FhirClient.UpdateAsync{T}(T,string,System.Threading.CancellationToken)"/>, by assigning a new GUID to the Id property.
        /// This is sometimes desirable over calling <see cref="FhirClient.CreateAsync{T}(T,string,System.Threading.CancellationToken)"/> when you want to be sure that at most
        /// one resource is created, even if the call has to be issued multiple times.
        /// </summary>
        public static Task<FhirResponse<T>> CreateByUpdateAsync<T>(this FhirClient client, T resource, CancellationToken cancellationToken = default)
            where T : Resource
        {
            resource.Id = Guid.NewGuid().ToString();

            return client.UpdateAsync(resource, cancellationToken: cancellationToken);
        }
    }
}
