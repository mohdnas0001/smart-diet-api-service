import { Test, TestingModule } from '@nestjs/testing';
import { INestApplication, Module } from '@nestjs/common';
import request from 'supertest';
import { AppController } from '../src/app.controller';
import { AppService } from '../src/app.service';

@Module({
  controllers: [AppController],
  providers: [AppService],
})
class TestAppModule {}

describe('Health endpoint (e2e)', () => {
  let app: INestApplication;

  beforeEach(async () => {
    const moduleFixture: TestingModule = await Test.createTestingModule({
      imports: [TestAppModule],
    }).compile();

    app = moduleFixture.createNestApplication();
    await app.init();
  });

  afterEach(async () => {
    await app.close();
  });

  it('/health (GET)', () => {
    const server = app.getHttpServer() as Parameters<typeof request>[0];

    return request(server).get('/health').expect(200).expect({
      status: 'ok',
      service: 'smart-diet-api-service',
      docs: '/docs',
    });
  });
});
