import { Injectable } from '@nestjs/common';

@Injectable()
export class AppService {
  getHealth() {
    return {
      status: 'ok',
      service: 'smart-diet-api-service',
      docs: '/docs',
    };
  }
}
