import {
  BadRequestException,
  Injectable,
  NotFoundException,
} from '@nestjs/common';
import { InjectRepository } from '@nestjs/typeorm';
import { Repository } from 'typeorm';
import { MlService } from '../ml/ml.service';
import { UsersService } from '../users/users.service';
import { Analysis } from './entities/analysis.entity';

@Injectable()
export class AnalysisService {
  constructor(
    @InjectRepository(Analysis)
    private readonly analysisRepository: Repository<Analysis>,
    private readonly mlService: MlService,
    private readonly usersService: UsersService,
  ) {}

  async create(userId: string, file?: Express.Multer.File) {
    if (!file) {
      throw new BadRequestException('Image file is required');
    }

    if (!file.mimetype.startsWith('image/')) {
      throw new BadRequestException('Only image uploads are supported');
    }

    await this.usersService.getProfileOrThrow(userId);
    const normalizedResult = await this.mlService.analyzeImage(file);

    const analysis = this.analysisRepository.create({
      userId,
      imageReference: file.originalname,
      detectedFoods: normalizedResult.detectedFoods,
      nutrients: normalizedResult.nutrients,
      totalCalories: normalizedResult.totalCalories,
      rawMlResponse: normalizedResult.rawMlResponse,
    });

    const savedAnalysis = await this.analysisRepository.save(analysis);
    return this.toResponse(savedAnalysis);
  }

  async findHistory(userId: string) {
    const analyses = await this.analysisRepository.find({
      where: { userId },
      order: { createdAt: 'DESC' },
    });

    return analyses.map((analysis) => this.toResponse(analysis));
  }

  async findOne(userId: string, analysisId: string) {
    const analysis = await this.analysisRepository.findOne({
      where: { id: analysisId, userId },
    });

    if (!analysis) {
      throw new NotFoundException('Analysis not found');
    }

    return this.toResponse(analysis);
  }

  private toResponse(analysis: Analysis) {
    return {
      id: analysis.id,
      userId: analysis.userId,
      imageReference: analysis.imageReference,
      detectedFoods: analysis.detectedFoods,
      nutrients: analysis.nutrients,
      totalCalories: analysis.totalCalories,
      rawMlResponse: analysis.rawMlResponse,
      createdAt: analysis.createdAt,
    };
  }
}
