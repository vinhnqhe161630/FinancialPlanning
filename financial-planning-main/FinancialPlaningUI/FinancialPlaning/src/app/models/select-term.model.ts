export class SelectTermModel {
    termId: string;
    termName: string;
    startDate: string; // Assuming date is represented as string in the format YYYY-MM-DD
    duration: number;
    reportDueDate: string; // Assuming date is represented as string in the format YYYY-MM-DD
    planDueDate: string; // Assuming date is represented as string in the format YYYY-MM-DD

  
    constructor(data: any = {}) {
        this.termId = data.termId || '';
        this.termName = data.termName || '';
        this.startDate = data.startDate || '';
        this.duration = data.duration || 1;
        this.reportDueDate = data.reportDueDate || '';
        this.planDueDate = data.planDueDate || '';
    }
  }