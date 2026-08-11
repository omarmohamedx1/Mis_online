import { apiClient } from '../../../services/apiClient';
import { isMasterDataCategory, type MasterDataCategory, type MasterDataItem, type MasterDataLookup, type MasterDataQuery, type PagedMasterData, type SaveMasterDataRequest } from '../types/masterData';

export const hrMasterDataService = {
  async getCategories(): Promise<MasterDataCategory[]> {
    const { data } = await apiClient.get<string[]>('/hr/master/categories');
    return data.filter(isMasterDataCategory);
  },

  async getPaged(query: MasterDataQuery): Promise<PagedMasterData> {
    const { data } = await apiClient.get<PagedMasterData>(`/hr/master/${query.category}`, {
      params: {
        isActive: query.isActive ?? undefined,
        page: query.page,
        pageSize: query.pageSize,
        search: query.search || undefined,
      },
    });
    return data;
  },

  async getLookup(category: MasterDataCategory, includeInactive = false): Promise<MasterDataLookup[]> {
    const { data } = await apiClient.get<MasterDataLookup[]>(`/hr/master/${category}/lookup`, { params: { includeInactive } });
    return data;
  },

  async getById(category: MasterDataCategory, id: string): Promise<MasterDataItem> {
    const { data } = await apiClient.get<MasterDataItem>(`/hr/master/${category}/${id}`);
    return data;
  },

  async create(category: MasterDataCategory, request: SaveMasterDataRequest): Promise<MasterDataItem> {
    const { data } = await apiClient.post<MasterDataItem>(`/hr/master/${category}`, request);
    return data;
  },

  async update(category: MasterDataCategory, id: string, request: SaveMasterDataRequest): Promise<MasterDataItem> {
    const { data } = await apiClient.put<MasterDataItem>(`/hr/master/${category}/${id}`, request);
    return data;
  },

  async setActive(category: MasterDataCategory, id: string, isActive: boolean): Promise<MasterDataItem> {
    const { data } = await apiClient.patch<MasterDataItem>(`/hr/master/${category}/${id}/active`, { isActive });
    return data;
  },
};
