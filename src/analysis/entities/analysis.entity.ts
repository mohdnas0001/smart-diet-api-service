import type {
  DetectedFood,
  NutrientBreakdown,
} from '../interfaces/analysis-result.interface';
import { User } from '../../users/entities/user.entity';
import {
  Column,
  CreateDateColumn,
  Entity,
  JoinColumn,
  ManyToOne,
  PrimaryGeneratedColumn,
} from 'typeorm';

@Entity('analyses')
export class Analysis {
  @PrimaryGeneratedColumn('uuid')
  id!: string;

  @Column({ type: 'uuid' })
  userId!: string;

  @ManyToOne(() => User, (user) => user.analyses, { onDelete: 'CASCADE' })
  @JoinColumn({ name: 'userId' })
  user!: User;

  @Column({ type: 'varchar', length: 255, nullable: true })
  imageReference!: string | null;

  @Column({ type: 'jsonb' })
  detectedFoods!: DetectedFood[];

  @Column({ type: 'jsonb' })
  nutrients!: NutrientBreakdown;

  @Column({ type: 'double precision' })
  totalCalories!: number;

  @Column({ type: 'jsonb', nullable: true })
  rawMlResponse!: Record<string, unknown> | null;

  @CreateDateColumn()
  createdAt!: Date;
}
