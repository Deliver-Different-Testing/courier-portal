// TODO: Implement fleet service — stubs for build

export interface Fleet {
  id: number;
  name: string;
}

export const fleetService = {
  async getAll(): Promise<Fleet[]> {
    return [];
  },
  async getById(_id: number): Promise<Fleet | null> {
    return null;
  },
  async sendMessage(_courierId: number, _message: string) {
    return { success: true };
  },
};
