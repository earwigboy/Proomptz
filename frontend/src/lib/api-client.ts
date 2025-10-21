/**
 * API Client Wrapper
 *
 * This file wraps the auto-generated OpenAPI client to provide a simpler interface.
 * The generated client is in ./api/ and is regenerated when running `npm run generate:api`
 *
 * To regenerate the API client after backend changes:
 * 1. Build the backend (which updates shared/openapi/swagger.json)
 * 2. Run: npm run generate:api
 */

import { OpenAPI } from './api/core/OpenAPI';
import { TemplatesService } from './api/services/TemplatesService';
import { FoldersService } from './api/services/FoldersService';

// Configure the API base URL
// In production (containerized), the API is served from the same origin
// In development, the API runs on a separate port (5026)
OpenAPI.BASE = import.meta.env.DEV
  ? 'http://localhost:5026'  // Development: Vite dev server -> separate backend
  : window.location.origin;   // Production: Same origin (backend serves frontend)

// Re-export all types from the generated API
export type {
  CreateTemplateRequest,
  UpdateTemplateRequest,
  TemplateResponse,
  TemplateSummary,
  TemplateListResponse,
  CreateFolderRequest,
  UpdateFolderRequest,
  FolderResponse,
  FolderTreeNode,
  FolderTreeResponse,
  FolderDetailsResponse,
} from './api';

// Create type aliases for backward compatibility
export type Template = import('./api/models/TemplateResponse').TemplateResponse;
export type Folder = import('./api/models/FolderResponse').FolderResponse;

/**
 * Templates API
 */
export const templatesApi = {
  getAll: async (params?: { folderId?: string; page?: number; pageSize?: number }) => {
    return TemplatesService.getApiTemplates({
      folderId: params?.folderId,
      page: params?.page,
      pageSize: params?.pageSize,
    });
  },

  getById: async (id: string) => {
    return TemplatesService.getApiTemplates1({ id });
  },

  create: async (data: import('./api/models/CreateTemplateRequest').CreateTemplateRequest) => {
    return TemplatesService.postApiTemplates({ requestBody: data });
  },

  update: async (id: string, data: import('./api/models/UpdateTemplateRequest').UpdateTemplateRequest) => {
    return TemplatesService.putApiTemplates({ id, requestBody: data });
  },

  delete: async (id: string) => {
    return TemplatesService.deleteApiTemplates({ id });
  },
};

/**
 * Folders API
 */
export const foldersApi = {
  getTree: async () => {
    return FoldersService.getApiFoldersTree();
  },

  getById: async (id: string) => {
    return FoldersService.getApiFolders({ id });
  },

  getChildren: async (parentId?: string) => {
    return FoldersService.getApiFolders1({ parentId });
  },

  create: async (data: import('./api/models/CreateFolderRequest').CreateFolderRequest) => {
    return FoldersService.postApiFolders({ requestBody: data });
  },

  update: async (id: string, data: import('./api/models/UpdateFolderRequest').UpdateFolderRequest) => {
    return FoldersService.putApiFolders({ id, requestBody: data });
  },

  delete: async (id: string) => {
    return FoldersService.deleteApiFolders({ id });
  },
};

// Export the OpenAPI configuration if needed for advanced usage
export { OpenAPI };
