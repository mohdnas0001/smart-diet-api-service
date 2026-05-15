import {
  BadGatewayException,
  GatewayTimeoutException,
  Injectable,
} from '@nestjs/common';
import { HttpService } from '@nestjs/axios';
import { ConfigService } from '@nestjs/config';
import { isAxiosError } from 'axios';
import FormData from 'form-data';
import { lastValueFrom } from 'rxjs';
import { NormalizedAnalysisResult } from '../analysis/interfaces/analysis-result.interface';
import { normalizeMlResponse } from './ml.mapper';

const extractUpstreamMessage = (value: unknown): string | undefined => {
  if (typeof value === 'string') {
    return value;
  }

  if (typeof value === 'object' && value !== null && 'message' in value) {
    const message = value.message;
    return typeof message === 'string' ? message : undefined;
  }

  return undefined;
};

@Injectable()
export class MlService {
  constructor(
    private readonly httpService: HttpService,
    private readonly configService: ConfigService,
  ) {}

  async analyzeImage(
    file: Express.Multer.File,
  ): Promise<NormalizedAnalysisResult> {
    const endpoint = this.configService.getOrThrow<string>(
      'ML_ANALYSIS_ENDPOINT',
    );
    const uploadFieldName =
      this.configService.get<string>('ML_ANALYSIS_FILE_FIELD') || 'file';
    const formData = new FormData();
    formData.append(uploadFieldName, file.buffer, {
      filename: file.originalname,
      contentType: file.mimetype,
    });

    try {
      const response = await lastValueFrom(
        this.httpService.post(endpoint, formData, {
          headers: formData.getHeaders(),
        }),
      );

      return normalizeMlResponse(response.data);
    } catch (error) {
      if (isAxiosError(error)) {
        if (error.code === 'ECONNABORTED') {
          throw new GatewayTimeoutException('ML service request timed out');
        }
        const upstreamMessage = extractUpstreamMessage(error.response?.data);
        throw new BadGatewayException(
          upstreamMessage ?? 'ML service request failed',
        );
      }

      throw new BadGatewayException('ML service request failed');
    }
  }
}
