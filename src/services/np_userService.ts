// TODO: Implement user service — stubs for build

export const userService = {
  getAll() {
    return [];
  },
  async create(_data: Record<string, unknown>) {
    return { success: true };
  },
  async invite(_email: string, _role?: string) {
    return { success: true };
  },
  async update(_id: number, _data: Record<string, unknown>) {
    return { success: true };
  },
  async delete(_id: number) {
    return { success: true };
  },
};
