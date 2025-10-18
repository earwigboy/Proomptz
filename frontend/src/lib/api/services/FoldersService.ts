/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { CreateFolderRequest } from '../models/CreateFolderRequest';
import type { FolderDetailsResponse } from '../models/FolderDetailsResponse';
import type { FolderResponse } from '../models/FolderResponse';
import type { FolderTreeResponse } from '../models/FolderTreeResponse';
import type { UpdateFolderRequest } from '../models/UpdateFolderRequest';
import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';
export class FoldersService {
    /**
     * @returns FolderTreeResponse OK
     * @throws ApiError
     */
    public static getApiFoldersTree(): CancelablePromise<FolderTreeResponse> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/Folders/tree',
        });
    }
    /**
     * @returns FolderDetailsResponse OK
     * @throws ApiError
     */
    public static getApiFolders({
        id,
    }: {
        id: string,
    }): CancelablePromise<FolderDetailsResponse> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/Folders/{id}',
            path: {
                'id': id,
            },
        });
    }
    /**
     * @returns FolderResponse OK
     * @throws ApiError
     */
    public static putApiFolders({
        id,
        requestBody,
    }: {
        id: string,
        requestBody?: UpdateFolderRequest,
    }): CancelablePromise<FolderResponse> {
        return __request(OpenAPI, {
            method: 'PUT',
            url: '/api/Folders/{id}',
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
    public static deleteApiFolders({
        id,
    }: {
        id: string,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'DELETE',
            url: '/api/Folders/{id}',
            path: {
                'id': id,
            },
        });
    }
    /**
     * @returns FolderResponse OK
     * @throws ApiError
     */
    public static getApiFolders1({
        parentId,
    }: {
        parentId?: string,
    }): CancelablePromise<Array<FolderResponse>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/Folders',
            query: {
                'parentId': parentId,
            },
        });
    }
    /**
     * @returns FolderResponse OK
     * @throws ApiError
     */
    public static postApiFolders({
        requestBody,
    }: {
        requestBody?: CreateFolderRequest,
    }): CancelablePromise<FolderResponse> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/api/Folders',
            body: requestBody,
            mediaType: 'application/json',
        });
    }
}
