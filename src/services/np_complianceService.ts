// TODO: Implement compliance service — stubs for build

export const complianceService = {
  async getOverview() {
    return [];
  },
  async getDetail(_courierId: number) {
    return { documents: [], issues: [] };
  },
  async getProfiles() {
    return [];
  },
  async getDocumentTypes() {
    return [];
  },
  async getCourierCompliance(_courierId: number) {
    return { compliant: false, documents: [], expiringDocuments: 0, missingDocuments: 0 };
  },
};
