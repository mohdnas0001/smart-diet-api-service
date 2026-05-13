import { HttpModule } from '@nestjs/axios';
import { Module } from '@nestjs/common';
import { ConfigModule, ConfigService } from '@nestjs/config';
import { MlService } from './ml.service';

@Module({
  imports: [
    ConfigModule,
    HttpModule.registerAsync({
      imports: [ConfigModule],
      inject: [ConfigService],
      useFactory: (configService: ConfigService) => ({
        baseURL: configService.getOrThrow<string>('ML_SERVICE_URL'),
        timeout: configService.getOrThrow<number>('ML_SERVICE_TIMEOUT_MS'),
      }),
    }),
  ],
  providers: [MlService],
  exports: [MlService],
})
export class MlModule {}
