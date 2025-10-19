/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { CreateTemplateRequest } from '../models/CreateTemplateRequest';
import type { GeneratePromptRequest } from '../models/GeneratePromptRequest';
import type { PlaceholderListResponse } from '../models/PlaceholderListResponse';
import type { PromptInstanceResponse } from '../models/PromptInstanceResponse';
import type { SendPromptResponse } from '../models/SendPromptResponse';
import type { TemplateListResponse } from '../models/TemplateListResponse';
import type { TemplateResponse } from '../models/TemplateResponse';
import type { UpdateTemplateRequest } from '../models/UpdateTemplateRequest';
import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';
export class TemplatesService {
    /**
     * @returns TemplateListResponse OK
     * @throws ApiError
     */
    public static getApiTemplates({
        folderId,
        page = 1,
        pageSize = 50,
    }: {
        folderId?: string,
        page?: number,
        pageSize?: number,
    }): CancelablePromise<TemplateListResponse> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/Templates',
            query: {
                'folderId': folderId,
                'page': page,
                'pageSize': pageSize,
            },
        });
    }
    /**
     * @returns TemplateResponse OK
     * @throws ApiError
     */
    public static postApiTemplates({
        requestBody,
    }: {
        requestBody?: CreateTemplateRequest,
    }): CancelablePromise<TemplateResponse> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/api/Templates',
            body: requestBody,
            mediaType: 'application/json',
        });
    }
    /**
     * @returns TemplateResponse OK
     * @throws ApiError
     */
    public static getApiTemplates1({
        id,
    }: {
        id: string,
    }): CancelablePromise<TemplateResponse> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/Templates/{id}',
            path: {
                'id': id,
            },
        });
    }
    /**
     * @returns TemplateResponse OK
     * @throws ApiError
     */
    public static putApiTemplates({
        id,
        requestBody,
    }: {
        id: string,
        requestBody?: UpdateTemplateRequest,
    }): CancelablePromise<TemplateResponse> {
        return __request(OpenAPI, {
            method: 'PUT',
            url: '/api/Templates/{id}',
            path: {
                'id': id,
            },
            body: requestBody,
            mediaType: 'application/json',
        });
    }
    /**
     * @returns any OK
     * @throws ApiError
     */
    public static deleteApiTemplates({
        id,
    }: {
        id: string,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'DELETE',
            url: '/api/Templates/{id}',
            path: {
                'id': id,
            },
        });
    }
    /**
     * @returns PlaceholderListResponse OK
     * @throws ApiError
     */
    public static getApiTemplatesPlaceholders({
        id,
    }: {
        id: string,
    }): CancelablePromise<PlaceholderListResponse> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/Templates/{id}/placeholders',
            path: {
                'id': id,
            },
        });
    }
    /**
     * @returns PromptInstanceResponse OK
     * @throws ApiError
     */
    public static postApiTemplatesGenerate({
        id,
        requestBody,
    }: {
        id: string,
        requestBody?: GeneratePromptRequest,
    }): CancelablePromise<PromptInstanceResponse> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/api/Templates/{id}/generate',
            path: {
                'id': id,
            },
            body: requestBody,
            mediaType: 'application/json',
        });
    }
    /**
     * @returns SendPromptResponse OK
     * @throws ApiError
     */
    public static postApiTemplatesSend({
        id,
        requestBody,
    }: {
        id: string,
        requestBody?: GeneratePromptRequest,
    }): CancelablePromise<SendPromptResponse> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/api/Templates/{id}/send',
            path: {
                'id': id,
            },
            body: requestBody,
            mediaType: 'application/json',
        });
    }
}
