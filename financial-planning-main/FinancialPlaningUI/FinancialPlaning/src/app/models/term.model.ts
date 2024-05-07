export class CreateTermModel {
    termName: string;
    creatorId: string;
    duration: number;
    startDate: string; // Assuming date is represented as string in the format YYYY-MM-DD
    planDueDate: string; // Assuming date is represented as string in the format YYYY-MM-DD
    reportDueDate: string; // Assuming date is represented as string in the format YYYY-MM-DD
  
    constructor(data: any = {}) {
      this.termName = data.termName || '';
      this.creatorId = data.creatorId || '';
      this.duration = data.duration || 1;
      this.startDate = data.startDate || '';
      this.planDueDate = data.planDueDate || '';
      this.reportDueDate = data.reportDueDate || '';
    }
  }
  