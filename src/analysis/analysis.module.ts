import { Module } from '@nestjs/common';
import { TypeOrmModule } from '@nestjs/typeorm';
import { CommonModule } from '../common/common.module';
import { MlModule } from '../ml/ml.module';
import { UsersModule } from '../users/users.module';
import { AnalysisController } from './analysis.controller';
import { AnalysisService } from './analysis.service';
import { Analysis } from './entities/analysis.entity';

@Module({
  imports: [
    TypeOrmModule.forFeature([Analysis]),
    MlModule,
    UsersModule,
    CommonModule,
  ],
  controllers: [AnalysisController],
  providers: [AnalysisService],
  exports: [AnalysisService],
})
export class AnalysisModule {}
