import { Injectable, UnauthorizedException } from '@nestjs/common';
import { ConfigService } from '@nestjs/config';
import { JwtService } from '@nestjs/jwt';
import * as bcrypt from 'bcrypt';
import type { StringValue } from 'ms';
import { User } from '../users/entities/user.entity';
import { UsersService } from '../users/users.service';
import { LoginDto } from './dto/login.dto';
import { RefreshTokenDto } from './dto/refresh-token.dto';
import { RegisterDto } from './dto/register.dto';
import { JwtPayload } from './interfaces/jwt-payload.interface';

@Injectable()
export class AuthService {
  constructor(
    private readonly usersService: UsersService,
    private readonly jwtService: JwtService,
    private readonly configService: ConfigService,
  ) {}

  async register(registerDto: RegisterDto) {
    const saltRounds =
      this.configService.getOrThrow<number>('BCRYPT_SALT_ROUNDS');
    const passwordHash = await bcrypt.hash(registerDto.password, saltRounds);

    const user = await this.usersService.create({
      name: registerDto.name,
      email: registerDto.email,
      passwordHash,
      age: registerDto.age,
      weight: registerDto.weight,
      height: registerDto.height,
      gender: registerDto.gender,
      activityLevel: registerDto.activityLevel,
      dietaryGoal: registerDto.dietaryGoal,
    });

    return this.buildAuthResponse(user);
  }

  async login(loginDto: LoginDto) {
    const user = await this.validateUser(loginDto.email, loginDto.password);
    return this.buildAuthResponse(user);
  }

  async refresh(refreshTokenDto: RefreshTokenDto) {
    const refreshSecret =
      this.configService.getOrThrow<string>('JWT_REFRESH_SECRET');
    const payload = await this.jwtService.verifyAsync<JwtPayload>(
      refreshTokenDto.refreshToken,
      {
        secret: refreshSecret,
      },
    );

    const user = await this.usersService.findById(payload.sub, true);
    if (!user?.refreshTokenHash) {
      throw new UnauthorizedException('Refresh token is not active');
    }

    const isRefreshTokenValid = await bcrypt.compare(
      refreshTokenDto.refreshToken,
      user.refreshTokenHash,
    );
    if (!isRefreshTokenValid) {
      throw new UnauthorizedException('Invalid refresh token');
    }

    return this.buildAuthResponse(user);
  }

  async logout(userId: string) {
    await this.usersService.updateRefreshTokenHash(userId, null);
    return {
      message: 'Logged out successfully',
    };
  }

  private async validateUser(email: string, password: string) {
    const user = await this.usersService.findByEmail(email, true);
    if (!user?.passwordHash) {
      throw new UnauthorizedException('Invalid email or password');
    }

    const isPasswordValid = await bcrypt.compare(password, user.passwordHash);
    if (!isPasswordValid) {
      throw new UnauthorizedException('Invalid email or password');
    }

    return user;
  }

  private async buildAuthResponse(user: User) {
    const tokens = await this.generateTokens(user);
    const saltRounds =
      this.configService.getOrThrow<number>('BCRYPT_SALT_ROUNDS');
    const refreshTokenHash = await bcrypt.hash(tokens.refreshToken, saltRounds);
    await this.usersService.updateRefreshTokenHash(user.id, refreshTokenHash);

    const safeUser = await this.usersService.getProfileOrThrow(user.id);

    return {
      user: safeUser,
      ...tokens,
    };
  }

  private async generateTokens(user: User) {
    const payload: JwtPayload = { sub: user.id, email: user.email };
    const accessTokenSecret =
      this.configService.getOrThrow<string>('JWT_SECRET');
    const refreshTokenSecret =
      this.configService.getOrThrow<string>('JWT_REFRESH_SECRET');
    const accessTokenExpiresIn =
      this.configService.getOrThrow<StringValue>('JWT_EXPIRES_IN');
    const refreshTokenExpiresIn = this.configService.getOrThrow<StringValue>(
      'JWT_REFRESH_EXPIRES_IN',
    );

    const [accessToken, refreshToken] = await Promise.all([
      this.jwtService.signAsync(payload, {
        secret: accessTokenSecret,
        expiresIn: accessTokenExpiresIn,
      }),
      this.jwtService.signAsync(payload, {
        secret: refreshTokenSecret,
        expiresIn: refreshTokenExpiresIn,
      }),
    ]);

    return {
      accessToken,
      refreshToken,
    };
  }
}
