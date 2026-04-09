// TODO: Implement recruitment service — stubs for build

export const recruitmentService = {
  async getAll() {
    return [];
  },
  async getById(_id: number) {
    return null;
  },
  async updateStatus(_id: number, _status: string, _notes?: string) {
    return { success: true };
  },
};
