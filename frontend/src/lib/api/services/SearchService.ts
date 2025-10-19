/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { TemplateListResponse } from '../models/TemplateListResponse';
import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';
export class SearchService {
    /**
     * @returns TemplateListResponse OK
     * @throws ApiError
     */
    public static getApiSearch({
        q,
        page = 1,
        pageSize = 50,
    }: {
        q?: string,
        page?: number,
        pageSize?: number,
    }): CancelablePromise<TemplateListResponse> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/Search',
            query: {
                'q': q,
                'page': page,
                'pageSize': pageSize,
            },
        });
    }
}
