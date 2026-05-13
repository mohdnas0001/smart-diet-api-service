import { Body, Controller, Get, Patch, UseGuards } from '@nestjs/common';
import { ApiBearerAuth, ApiOkResponse, ApiTags } from '@nestjs/swagger';
import type { JwtUser } from '../auth/interfaces/jwt-user.interface';
import { CurrentUser } from '../common/decorators/current-user.decorator';
import { JwtAuthGuard } from '../common/guards/jwt-auth.guard';
import { UpdateProfileDto } from './dto/update-profile.dto';
import { UsersService } from './users.service';

@ApiTags('users')
@ApiBearerAuth()
@UseGuards(JwtAuthGuard)
@Controller('users')
export class UsersController {
  constructor(private readonly usersService: UsersService) {}

  @Get('profile')
  @ApiOkResponse({ description: 'Get the authenticated user profile.' })
  async getProfile(@CurrentUser() user: JwtUser) {
    return {
      user: await this.usersService.getProfileOrThrow(user.sub),
    };
  }

  @Patch('profile')
  @ApiOkResponse({ description: 'Update the authenticated user profile.' })
  async updateProfile(
    @CurrentUser() user: JwtUser,
    @Body() updateProfileDto: UpdateProfileDto,
  ) {
    return {
      user: await this.usersService.updateProfile(user.sub, updateProfileDto),
    };
  }
}
