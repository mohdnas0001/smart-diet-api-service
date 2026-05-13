import { AppController } from './app.controller';
import { AppService } from './app.service';

describe('AppController', () => {
  it('should return service health information', () => {
    const controller = new AppController(new AppService());

    expect(controller.getHealth()).toEqual({
      status: 'ok',
      service: 'smart-diet-api-service',
      docs: '/docs',
    });
  });
});
