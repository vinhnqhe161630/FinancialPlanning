export interface Report {
    id: string;
    reportName: string;
    month: number;
    status: number;
    termName: string;
    updateDate: Date;
    departmentName?: string;
    version: string;
  }
  