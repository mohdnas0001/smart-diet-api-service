import {
  ConflictException,
  Injectable,
  NotFoundException,
} from '@nestjs/common';
import { InjectRepository } from '@nestjs/typeorm';
import { Repository } from 'typeorm';
import { UpdateProfileDto } from './dto/update-profile.dto';
import { User } from './entities/user.entity';

interface CreateUserInput {
  name: string;
  email: string;
  passwordHash: string;
  age?: number;
  weight?: number;
  height?: number;
  gender?: string;
  activityLevel?: string;
  dietaryGoal?: string;
}

@Injectable()
export class UsersService {
  constructor(
    @InjectRepository(User)
    private readonly usersRepository: Repository<User>,
  ) {}

  async create(input: CreateUserInput) {
    const existingUser = await this.findByEmail(input.email, true);
    if (existingUser) {
      throw new ConflictException('Email is already registered');
    }

    const user = this.usersRepository.create({
      ...input,
      email: input.email.toLowerCase(),
      refreshTokenHash: null,
      age: input.age ?? null,
      weight: input.weight ?? null,
      height: input.height ?? null,
      gender: input.gender ?? null,
      activityLevel: input.activityLevel ?? null,
      dietaryGoal: input.dietaryGoal ?? null,
    });

    return this.usersRepository.save(user);
  }

  async findByEmail(email: string, includeSensitive = false) {
    if (includeSensitive) {
      return this.usersRepository
        .createQueryBuilder('user')
        .addSelect(['user.passwordHash', 'user.refreshTokenHash'])
        .where('LOWER(user.email) = LOWER(:email)', { email })
        .getOne();
    }

    return this.usersRepository.findOne({
      where: { email: email.toLowerCase() },
    });
  }

  async findById(id: string, includeSensitive = false) {
    if (includeSensitive) {
      return this.usersRepository
        .createQueryBuilder('user')
        .addSelect(['user.passwordHash', 'user.refreshTokenHash'])
        .where('user.id = :id', { id })
        .getOne();
    }

    return this.usersRepository.findOne({ where: { id } });
  }

  async getProfileOrThrow(userId: string) {
    const user = await this.findById(userId);
    if (!user) {
      throw new NotFoundException('User not found');
    }

    return this.toSafeUser(user);
  }

  async updateProfile(userId: string, updateProfileDto: UpdateProfileDto) {
    const user = await this.findById(userId);
    if (!user) {
      throw new NotFoundException('User not found');
    }

    Object.assign(user, updateProfileDto);
    const savedUser = await this.usersRepository.save(user);
    return this.toSafeUser(savedUser);
  }

  async updateRefreshTokenHash(
    userId: string,
    refreshTokenHash: string | null,
  ) {
    await this.usersRepository.update(userId, { refreshTokenHash });
  }

  toSafeUser(user: User) {
    return {
      id: user.id,
      name: user.name,
      email: user.email,
      age: user.age,
      weight: user.weight,
      height: user.height,
      gender: user.gender,
      activityLevel: user.activityLevel,
      dietaryGoal: user.dietaryGoal,
      createdAt: user.createdAt,
      updatedAt: user.updatedAt,
    };
  }
}
